"""
Minimal repro for: Copilot SDK headless mode does not load LSP config from .github/lsp.json
even when the CLI subprocess cwd is set to the repo root.

Expected: LSP tools (e.g. go_to_definition, get_document_symbols) should be available.
Actual:   Only built-in tools (grep, glob, view, etc.) are available; model falls back to grep.

Repo: https://github.com/ericstj/runtime/tree/lspTest
      (has .github/lsp.json with C# Roslyn LSP server config)

Workaround: running `copilot` directly from this directory DOES load LSP tools correctly.

SDK issue: https://github.com/github/copilot-sdk/issues/909
"""

import asyncio
import sys
from pathlib import Path

# Requires the 'copilot' SDK package. Install with:
#   pip install copilot
from copilot import CopilotClient, PermissionHandler
from copilot.session import SessionEventType

REPO_DIR = str(Path(__file__).parent)
QUESTION = (
    "Using only LSP tools, in System.Diagnostics.Process, when passing STARTUPINFOW "
    "to CreateProcessW via P/Invoke, which field controls which handles are inherited "
    "by the child process, and where is the corresponding P/Invoke defined?"
)


async def main():
    print(f"Repo dir (cwd for CLI subprocess): {REPO_DIR}")
    print(f"LSP config: {REPO_DIR}\\.github\\lsp.json")
    print()

    # Set cwd so CLI subprocess is launched from repo root,
    # which should trigger auto-discovery of .github/lsp.json
    client = CopilotClient({"cwd": REPO_DIR})

    await client.start()
    try:
        session = await client.create_session({
            "on_permission_request": PermissionHandler.approve_all,
            "working_directory": REPO_DIR,
        })

        tool_calls = []

        def on_event(event):
            if event.type == SessionEventType.TOOL_EXECUTION_START:
                tool_calls.append(event.data.tool_name)
                print(f"  [tool] {event.data.tool_name}", flush=True)

        session.on(on_event)

        print(f"Asking: {QUESTION[:80]}...")
        print()

        response = await session.send_and_wait({"prompt": QUESTION}, timeout=120)
        answer = response.data.content if response and response.data else ""

        print()
        print("=" * 60)
        print(f"Tools used ({len(tool_calls)}): {tool_calls}")
        print()

        lsp_tools = [t for t in tool_calls if t not in (
            "grep", "glob", "view", "read_file", "list_files",
            "run_command", "report_intent", "write_file", "create_file",
        )]
        if lsp_tools:
            print(f"✅ LSP tools detected: {lsp_tools}")
        else:
            print("❌ No LSP tools used — model fell back to built-in tools")
            print("   (Expected LSP tools like go_to_definition, get_document_symbols, etc.)")

        print()
        print("Answer (first 500 chars):")
        print(answer[:500])
    finally:
        await client.stop()


if __name__ == "__main__":
    asyncio.run(main())

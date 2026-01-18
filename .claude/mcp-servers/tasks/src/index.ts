// ============================================================================
// index.ts
// Purpose: MCP server entry point - starts the task management server
// Dependencies: @modelcontextprotocol/sdk, server
// ============================================================================

import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import { createServer } from './server.js';

async function main() {
  const server = createServer();
  const transport = new StdioServerTransport();

  await server.connect(transport);

  // Log to stderr (stdout is reserved for MCP protocol)
  console.error('Task MCP server started');
  console.error('Working directory:', process.cwd());
}

main().catch((error) => {
  console.error('Fatal error:', error);
  process.exit(1);
});

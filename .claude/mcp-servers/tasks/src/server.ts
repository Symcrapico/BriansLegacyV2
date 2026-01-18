// ============================================================================
// server.ts
// Purpose: MCP server setup and tool registration
// Dependencies: @modelcontextprotocol/sdk, all tool modules
// ============================================================================

import { Server } from '@modelcontextprotocol/sdk/server/index.js';
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
  CallToolResult,
  TextContent,
} from '@modelcontextprotocol/sdk/types.js';

import { statusTool } from './tools/status.js';
import { listTool } from './tools/list.js';
import { currentTool } from './tools/current.js';
import { nextTool } from './tools/next.js';
import { getTool } from './tools/get.js';
import { createTool } from './tools/create.js';
import { startTool } from './tools/start.js';
import { completeTool } from './tools/complete.js';
import { blockTool } from './tools/block.js';
import { unblockTool } from './tools/unblock.js';
import { updateTool } from './tools/update.js';
import { deleteTool } from './tools/delete.js';
import { reorderTool } from './tools/reorder.js';
import { addEpicTool } from './tools/add-epic.js';
import { addFeatureTool } from './tools/add-feature.js';

// Tool registry
const tools: Record<string, { schema: any; handler: (input: any) => Promise<any> }> = {
  status: statusTool,
  list: listTool,
  current: currentTool,
  next: nextTool,
  get: getTool,
  create: createTool,
  start: startTool,
  complete: completeTool,
  block: blockTool,
  unblock: unblockTool,
  update: updateTool,
  delete: deleteTool,
  reorder: reorderTool,
  add_epic: addEpicTool,
  add_feature: addFeatureTool,
};

export function createServer(): Server {
  const server = new Server(
    {
      name: 'tasks',
      version: '1.0.0',
    },
    {
      capabilities: {
        tools: {},
      },
    }
  );

  // List available tools
  server.setRequestHandler(ListToolsRequestSchema, async () => {
    return {
      tools: Object.entries(tools).map(([name, tool]) => ({
        name,
        description: tool.schema.description,
        inputSchema: {
          type: 'object' as const,
          properties: tool.schema.inputSchema.shape
            ? Object.fromEntries(
                Object.entries(tool.schema.inputSchema.shape).map(
                  ([key, value]: [string, any]) => [
                    key,
                    {
                      type: getZodType(value),
                      description: value.description,
                    },
                  ]
                )
              )
            : {},
          required: tool.schema.inputSchema.shape
            ? Object.entries(tool.schema.inputSchema.shape)
                .filter(([_, value]: [string, any]) => !value.isOptional?.())
                .map(([key]) => key)
            : [],
        },
      })),
    };
  });

  // Handle tool calls
  server.setRequestHandler(CallToolRequestSchema, async (request): Promise<CallToolResult> => {
    const { name, arguments: args } = request.params;

    const tool = tools[name];
    if (!tool) {
      return {
        content: [{ type: 'text', text: `Unknown tool: ${name}` } as TextContent],
        isError: true,
      };
    }

    try {
      // Parse and validate input
      const parsed = tool.schema.inputSchema.parse(args || {});
      const result = await tool.handler(parsed);
      return result as CallToolResult;
    } catch (error) {
      return {
        content: [
          {
            type: 'text',
            text: `Error: ${(error as Error).message}`,
          } as TextContent,
        ],
        isError: true,
      };
    }
  });

  return server;
}

// Helper to convert Zod types to JSON Schema types
function getZodType(zodType: any): string {
  const typeName = zodType._def?.typeName;
  switch (typeName) {
    case 'ZodString':
      return 'string';
    case 'ZodNumber':
      return 'number';
    case 'ZodBoolean':
      return 'boolean';
    case 'ZodArray':
      return 'array';
    case 'ZodObject':
      return 'object';
    case 'ZodEnum':
      return 'string';
    case 'ZodOptional':
      return getZodType(zodType._def.innerType);
    case 'ZodDefault':
      return getZodType(zodType._def.innerType);
    default:
      return 'string';
  }
}

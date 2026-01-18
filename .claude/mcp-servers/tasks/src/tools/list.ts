// ============================================================================
// list.ts
// Purpose: MCP tool to list tasks with optional filters
// Dependencies: parser, utils, types
// ============================================================================

import { z } from 'zod';
import { parseBacklog, backlogExists } from '../parser.js';
import { STATUS, ToolResult, jsonResult, errorResult, Task } from '../types.js';

export const listTool = {
  schema: {
    description: 'List tasks with optional filters by status, epic, or feature',
    inputSchema: z.object({
      status: z
        .enum(['backlog', 'ready', 'in_progress', 'blocked', 'done'])
        .optional()
        .describe('Filter by status'),
      epic: z.string().optional().describe('Filter by epic ID (e.g., EPIC-001)'),
      feature: z
        .string()
        .optional()
        .describe('Filter by feature ID (e.g., FEAT-001-A)'),
    }),
  },

  handler: async (input: {
    status?: string;
    epic?: string;
    feature?: string;
  }): Promise<ToolResult> => {
    try {
      if (!backlogExists()) {
        return errorResult("BACKLOG.md not found. Run '/task init' first.");
      }

      const backlog = parseBacklog();
      let tasks: Task[] = [];

      // Collect all tasks
      for (const epic of backlog.epics) {
        if (input.epic && epic.id !== input.epic) continue;

        for (const feature of epic.features) {
          if (input.feature && feature.id !== input.feature) continue;

          tasks.push(...feature.tasks);
        }
      }

      // Filter by status if specified
      if (input.status) {
        const statusEmoji =
          STATUS[input.status.toUpperCase() as keyof typeof STATUS];
        if (statusEmoji) {
          tasks = tasks.filter((t) => t.status === statusEmoji);
        }
      }

      // Format output
      const result = {
        count: tasks.length,
        filters: {
          status: input.status || 'all',
          epic: input.epic || 'all',
          feature: input.feature || 'all',
        },
        tasks: tasks.map((t) => ({
          id: t.id,
          description: t.description,
          status: t.status,
          estimate: t.estimate,
          feature: t.featureId,
          epic: t.epicId,
        })),
      };

      return jsonResult(result);
    } catch (error) {
      return errorResult((error as Error).message);
    }
  },
};

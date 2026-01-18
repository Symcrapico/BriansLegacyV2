// ============================================================================
// next.ts
// Purpose: MCP tool to get the next N ready tasks
// Dependencies: parser, utils, types
// ============================================================================

import { z } from 'zod';
import { parseBacklog, backlogExists } from '../parser.js';
import { getNextReadyTasks } from '../utils.js';
import { ToolResult, jsonResult, errorResult } from '../types.js';

export const nextTool = {
  schema: {
    description: 'Get the next N ready tasks',
    inputSchema: z.object({
      count: z
        .number()
        .min(1)
        .max(20)
        .default(3)
        .describe('Number of tasks to return (default: 3)'),
    }),
  },

  handler: async (input: { count?: number }): Promise<ToolResult> => {
    try {
      if (!backlogExists()) {
        return errorResult("BACKLOG.md not found. Run '/task init' first.");
      }

      const backlog = parseBacklog();
      const count = input.count || 3;
      const tasks = getNextReadyTasks(backlog, count);

      if (tasks.length === 0) {
        return jsonResult({
          count: 0,
          message: 'No ready tasks. Add tasks or mark existing ones as ready.',
          tasks: [],
        });
      }

      return jsonResult({
        count: tasks.length,
        tasks: tasks.map((t, i) => ({
          priority: i + 1,
          id: t.id,
          description: t.description,
          estimate: t.estimate,
          feature: t.featureId,
          epic: t.epicId,
        })),
      });
    } catch (error) {
      return errorResult((error as Error).message);
    }
  },
};

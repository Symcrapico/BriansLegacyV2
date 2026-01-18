// ============================================================================
// reorder.ts
// Purpose: MCP tool to reorder tasks within a feature
// Dependencies: parser, writer, utils, types
// ============================================================================

import { z } from 'zod';
import { parseBacklog, backlogExists } from '../parser.js';
import { writeBacklog } from '../writer.js';
import { findTask, findFeatureForTask } from '../utils.js';
import { ToolResult, textResult, errorResult } from '../types.js';

export const reorderTool = {
  schema: {
    description: 'Reorder a task within its feature (move after another task)',
    inputSchema: z.object({
      id: z.string().describe('Task ID to move'),
      after: z
        .string()
        .optional()
        .describe('Task ID to place after (omit to move to top)'),
    }),
  },

  handler: async (input: { id: string; after?: string }): Promise<ToolResult> => {
    try {
      if (!backlogExists()) {
        return errorResult("BACKLOG.md not found. Run '/task init' first.");
      }

      const backlog = parseBacklog();
      const task = findTask(backlog, input.id);

      if (!task) {
        return errorResult(`Task ${input.id} not found.`);
      }

      const feature = findFeatureForTask(backlog, input.id);
      if (!feature) {
        return errorResult(`Could not find feature for task ${input.id}.`);
      }

      // Find current index
      const currentIndex = feature.tasks.findIndex((t) => t.id === input.id);
      if (currentIndex === -1) {
        return errorResult(`Task ${input.id} not in feature.`);
      }

      // Remove from current position
      const [movedTask] = feature.tasks.splice(currentIndex, 1);

      // Find new position
      let newIndex = 0;
      if (input.after) {
        const afterIndex = feature.tasks.findIndex((t) => t.id === input.after);
        if (afterIndex === -1) {
          return errorResult(
            `Task ${input.after} not found in the same feature.`
          );
        }
        newIndex = afterIndex + 1;
      }

      // Insert at new position
      feature.tasks.splice(newIndex, 0, movedTask);

      writeBacklog(backlog);

      const position = input.after ? `after ${input.after}` : 'to top';
      return textResult(
        `Moved ${input.id} ${position}\n` +
          `New order in ${feature.id}:\n` +
          feature.tasks.map((t, i) => `  ${i + 1}. ${t.id}`).join('\n')
      );
    } catch (error) {
      return errorResult((error as Error).message);
    }
  },
};

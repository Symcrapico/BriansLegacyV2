// ============================================================================
// update.ts
// Purpose: MCP tool to update task details
// Dependencies: parser, writer, utils, types
// ============================================================================

import { z } from 'zod';
import { parseBacklog, backlogExists } from '../parser.js';
import { writeBacklog } from '../writer.js';
import { findTask } from '../utils.js';
import { STATUS, ToolResult, textResult, errorResult } from '../types.js';

export const updateTool = {
  schema: {
    description: 'Update task description, estimate, or status',
    inputSchema: z.object({
      id: z.string().describe('Task ID to update'),
      description: z.string().optional().describe('New description'),
      estimate: z.string().optional().describe('New estimate (e.g., 30m, 1h)'),
      status: z
        .enum(['backlog', 'ready'])
        .optional()
        .describe('New status (only backlog or ready)'),
    }),
  },

  handler: async (input: {
    id: string;
    description?: string;
    estimate?: string;
    status?: 'backlog' | 'ready';
  }): Promise<ToolResult> => {
    try {
      if (!backlogExists()) {
        return errorResult("BACKLOG.md not found. Run '/task init' first.");
      }

      const backlog = parseBacklog();
      const task = findTask(backlog, input.id);

      if (!task) {
        return errorResult(`Task ${input.id} not found.`);
      }

      const changes: string[] = [];

      if (input.description) {
        task.description = input.description;
        changes.push(`description: "${input.description}"`);
      }

      if (input.estimate) {
        task.estimate = input.estimate;
        changes.push(`estimate: ${input.estimate}`);
      }

      if (input.status) {
        // Only allow changing to backlog or ready (not in_progress or blocked)
        const newStatus =
          input.status === 'backlog' ? STATUS.BACKLOG : STATUS.READY;
        task.status = newStatus;
        changes.push(`status: ${input.status}`);
      }

      if (changes.length === 0) {
        return errorResult('No changes specified.');
      }

      writeBacklog(backlog);

      return textResult(
        `Updated ${input.id}\n` + `Changes: ${changes.join(', ')}`
      );
    } catch (error) {
      return errorResult((error as Error).message);
    }
  },
};

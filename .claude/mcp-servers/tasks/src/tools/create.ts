// ============================================================================
// create.ts
// Purpose: MCP tool to create a new task in a feature
// Dependencies: parser, writer, utils, types
// ============================================================================

import { z } from 'zod';
import { parseBacklog, backlogExists } from '../parser.js';
import { writeBacklog } from '../writer.js';
import { findFeature, generateTaskId, recalculateCounts } from '../utils.js';
import { STATUS, Task, ToolResult, textResult, errorResult } from '../types.js';

export const createTool = {
  schema: {
    description: 'Create a new task in a feature',
    inputSchema: z.object({
      description: z.string().describe('Task description'),
      feature: z.string().describe('Feature ID (e.g., FEAT-001-A)'),
      estimate: z
        .string()
        .optional()
        .describe('Time estimate (e.g., 30m, 1h, 2h)'),
      status: z
        .enum(['backlog', 'ready'])
        .default('ready')
        .describe('Initial status (default: ready)'),
    }),
  },

  handler: async (input: {
    description: string;
    feature: string;
    estimate?: string;
    status?: 'backlog' | 'ready';
  }): Promise<ToolResult> => {
    try {
      if (!backlogExists()) {
        return errorResult("BACKLOG.md not found. Run '/task init' first.");
      }

      const backlog = parseBacklog();
      const feature = findFeature(backlog, input.feature);

      if (!feature) {
        return errorResult(
          `Feature ${input.feature} not found. Create it first with add_feature.`
        );
      }

      const taskId = generateTaskId(feature);
      const statusEmoji =
        input.status === 'backlog' ? STATUS.BACKLOG : STATUS.READY;

      const task: Task = {
        id: taskId,
        description: input.description,
        status: statusEmoji,
        estimate: input.estimate,
        featureId: feature.id,
        epicId: feature.epicId,
      };

      feature.tasks.push(task);
      recalculateCounts(backlog);
      writeBacklog(backlog);

      return textResult(
        `Created ${taskId}: ${input.description}\n` +
          `Feature: ${feature.id} (${feature.title})\n` +
          `Status: ${statusEmoji} ${input.status || 'ready'}`
      );
    } catch (error) {
      return errorResult((error as Error).message);
    }
  },
};

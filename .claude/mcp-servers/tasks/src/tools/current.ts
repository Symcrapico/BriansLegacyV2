// ============================================================================
// current.ts
// Purpose: MCP tool to get the currently active task
// Dependencies: parser, utils, types
// ============================================================================

import { z } from 'zod';
import { parseBacklog, backlogExists } from '../parser.js';
import { getActiveTask, findFeature, findEpic, timeSince } from '../utils.js';
import { ToolResult, jsonResult, textResult, errorResult } from '../types.js';

export const currentTool = {
  schema: {
    description: 'Get the currently active (in-progress) task',
    inputSchema: z.object({}),
  },

  handler: async (): Promise<ToolResult> => {
    try {
      if (!backlogExists()) {
        return errorResult("BACKLOG.md not found. Run '/task init' first.");
      }

      const backlog = parseBacklog();
      const active = getActiveTask(backlog);

      if (!active) {
        return textResult('No active task. Use mcp__tasks__start to begin one.');
      }

      const feature = findFeature(backlog, active.featureId);
      const epic = findEpic(backlog, active.epicId);

      const result = {
        ...active,
        startedAgo: active.started ? timeSince(active.started) : null,
        featureTitle: feature?.title,
        featureProgress: feature
          ? `${feature.completedCount}/${feature.totalCount}`
          : null,
        epicTitle: epic?.title,
        epicProgress: epic ? `${epic.completedCount}/${epic.totalCount}` : null,
      };

      return jsonResult(result);
    } catch (error) {
      return errorResult((error as Error).message);
    }
  },
};

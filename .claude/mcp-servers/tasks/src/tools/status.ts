// ============================================================================
// status.ts
// Purpose: MCP tool to get full backlog status including active, blocked, next
// Dependencies: parser, utils, types
// ============================================================================

import { z } from 'zod';
import { parseBacklog, backlogExists } from '../parser.js';
import {
  getActiveTask,
  getBlockedTasks,
  getNextReadyTasks,
  findFeature,
  findEpic,
  timeSince,
} from '../utils.js';
import { StatusResponse, ToolResult, jsonResult, errorResult } from '../types.js';

export const statusTool = {
  schema: {
    description:
      'Get full backlog status including active task, blocked tasks, next ready tasks, and progress',
    inputSchema: z.object({}),
  },

  handler: async (): Promise<ToolResult> => {
    try {
      if (!backlogExists()) {
        return errorResult(
          "BACKLOG.md not found. Run '/task init' to initialize task management."
        );
      }

      const backlog = parseBacklog();
      const active = getActiveTask(backlog);
      const blocked = getBlockedTasks(backlog);
      const next = getNextReadyTasks(backlog, 3);

      let progress = null;
      if (active) {
        const feature = findFeature(backlog, active.featureId);
        const epic = findEpic(backlog, active.epicId);
        if (feature && epic) {
          progress = {
            epic: epic.id,
            epicTitle: epic.title,
            epicProgress: `${epic.completedCount}/${epic.totalCount}`,
            feature: feature.id,
            featureTitle: feature.title,
            featureProgress: `${feature.completedCount}/${feature.totalCount}`,
          };
        }
      }

      const response: StatusResponse = {
        active: active
          ? {
              ...active,
              started: active.started
                ? `${active.started} (${timeSince(active.started)})`
                : undefined,
            }
          : null,
        blocked,
        next,
        progress,
      };

      return jsonResult(response);
    } catch (error) {
      return errorResult((error as Error).message);
    }
  },
};

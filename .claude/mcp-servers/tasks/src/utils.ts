// ============================================================================
// utils.ts
// Purpose: Helper functions for task lookup, ID generation, and calculations
// Dependencies: types
// ============================================================================

import { Backlog, Task, Feature, Epic, STATUS } from './types.js';

/**
 * Find a task by ID across all epics and features
 */
export function findTask(backlog: Backlog, taskId: string): Task | null {
  for (const epic of backlog.epics) {
    for (const feature of epic.features) {
      const task = feature.tasks.find((t) => t.id === taskId);
      if (task) return task;
    }
  }
  return null;
}

/**
 * Find a feature by ID
 */
export function findFeature(backlog: Backlog, featureId: string): Feature | null {
  for (const epic of backlog.epics) {
    const feature = epic.features.find((f) => f.id === featureId);
    if (feature) return feature;
  }
  return null;
}

/**
 * Find an epic by ID
 */
export function findEpic(backlog: Backlog, epicId: string): Epic | null {
  return backlog.epics.find((e) => e.id === epicId) || null;
}

/**
 * Find the feature containing a task
 */
export function findFeatureForTask(backlog: Backlog, taskId: string): Feature | null {
  for (const epic of backlog.epics) {
    for (const feature of epic.features) {
      if (feature.tasks.some((t) => t.id === taskId)) {
        return feature;
      }
    }
  }
  return null;
}

/**
 * Get the currently active task
 */
export function getActiveTask(backlog: Backlog): Task | null {
  if (!backlog.active) return null;
  return findTask(backlog, backlog.active);
}

/**
 * Get all blocked tasks
 */
export function getBlockedTasks(backlog: Backlog): Task[] {
  const blocked: Task[] = [];
  for (const epic of backlog.epics) {
    for (const feature of epic.features) {
      blocked.push(...feature.tasks.filter((t) => t.status === STATUS.BLOCKED));
    }
  }
  return blocked;
}

/**
 * Get next N ready tasks (in order of appearance)
 */
export function getNextReadyTasks(backlog: Backlog, count: number = 3): Task[] {
  const ready: Task[] = [];
  for (const epic of backlog.epics) {
    for (const feature of epic.features) {
      for (const task of feature.tasks) {
        if (task.status === STATUS.READY) {
          ready.push(task);
          if (ready.length >= count) {
            return ready;
          }
        }
      }
    }
  }
  return ready;
}

/**
 * Get all tasks with a specific status
 */
export function getTasksByStatus(backlog: Backlog, status: string): Task[] {
  const tasks: Task[] = [];
  for (const epic of backlog.epics) {
    for (const feature of epic.features) {
      tasks.push(...feature.tasks.filter((t) => t.status === status));
    }
  }
  return tasks;
}

/**
 * Generate next task ID for a feature
 */
export function generateTaskId(feature: Feature): string {
  const prefix = feature.id.replace('FEAT-', 'TASK-');
  const existingNumbers = feature.tasks.map((t) => {
    const match = t.id.match(/-(\d+)$/);
    return match ? parseInt(match[1]) : 0;
  });
  const nextNumber = Math.max(0, ...existingNumbers) + 1;
  return `${prefix}-${nextNumber}`;
}

/**
 * Generate next feature ID for an epic
 */
export function generateFeatureId(epic: Epic): string {
  const letters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
  const existingLetters = epic.features.map((f) => {
    const match = f.id.match(/-([A-Z])$/);
    return match ? match[1] : '';
  });
  const maxIndex = existingLetters
    .map((l) => letters.indexOf(l))
    .reduce((max, i) => Math.max(max, i), -1);
  const nextLetter = letters[maxIndex + 1] || 'Z';
  const epicNum = epic.id.replace('EPIC-', '');
  return `FEAT-${epicNum}-${nextLetter}`;
}

/**
 * Generate next epic ID
 */
export function generateEpicId(backlog: Backlog): string {
  const existingNumbers = backlog.epics.map((e) => {
    const match = e.id.match(/EPIC-(\d+)/);
    return match ? parseInt(match[1]) : 0;
  });
  const nextNumber = Math.max(0, ...existingNumbers) + 1;
  return `EPIC-${String(nextNumber).padStart(3, '0')}`;
}

/**
 * Update task counts for all features and epics
 */
export function recalculateCounts(backlog: Backlog): void {
  for (const epic of backlog.epics) {
    epic.completedCount = 0;
    epic.totalCount = 0;
    for (const feature of epic.features) {
      feature.totalCount = feature.tasks.length;
      feature.completedCount = feature.tasks.filter(
        (t) => t.status === STATUS.DONE
      ).length;
      epic.totalCount += feature.totalCount;
      epic.completedCount += feature.completedCount;
    }
  }
}

/**
 * Remove a task from its feature
 */
export function removeTask(backlog: Backlog, taskId: string): Task | null {
  for (const epic of backlog.epics) {
    for (const feature of epic.features) {
      const index = feature.tasks.findIndex((t) => t.id === taskId);
      if (index !== -1) {
        const [removed] = feature.tasks.splice(index, 1);
        return removed;
      }
    }
  }
  return null;
}

/**
 * Format a timestamp for display
 */
export function formatTimestamp(iso: string): string {
  try {
    const date = new Date(iso);
    return date.toISOString().replace('T', ' ').substring(0, 16);
  } catch {
    return iso;
  }
}

/**
 * Calculate time since a timestamp
 */
export function timeSince(iso: string): string {
  try {
    const date = new Date(iso);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffDays > 0) return `${diffDays}d ago`;
    if (diffHours > 0) return `${diffHours}h ago`;
    if (diffMins > 0) return `${diffMins}m ago`;
    return 'just now';
  } catch {
    return '';
  }
}

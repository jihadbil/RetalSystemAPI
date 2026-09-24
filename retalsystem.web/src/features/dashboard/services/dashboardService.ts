import { apiClient } from '../../../shared/services/apiClient';
import type { DashboardSummary } from '../types/dashboard.types';

export const dashboardService = {
  async getSummary(branchId?: string): Promise<DashboardSummary> {
    try {
      return await apiClient.get<DashboardSummary>('dashboard/summary', { branchId });
    } catch (err) {
      console.error('[dashboardService.getSummary] Failed to fetch summary:', err);
      throw err;
    }
  },
};

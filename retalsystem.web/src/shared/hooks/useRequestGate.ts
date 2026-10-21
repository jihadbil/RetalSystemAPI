import { useState } from 'react';

/** A stable request generation. The caller invalidates it when its query inputs change. */
export function useRequestGate() {
  return useState(() => {
    let generation = 0;
    return {
      begin() { const own = ++generation; return () => own === generation; },
      invalidate() { generation++; },
    };
  })[0];
}

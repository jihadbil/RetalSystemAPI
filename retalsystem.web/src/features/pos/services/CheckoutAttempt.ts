/** One key per attempted sale; a failed transport can retry the same payload safely. */
export class CheckoutAttempt {
  inFlight = false;
  private attempt: { fingerprint: string; key: string } | null = null;

  begin(payload: unknown): string | null {
    if (this.inFlight) return null;
    const fingerprint = JSON.stringify(payload);
    if (this.attempt?.fingerprint !== fingerprint) {
      this.attempt = { fingerprint, key: `POS-${crypto.randomUUID().replaceAll('-', '')}` };
    }
    this.inFlight = true;
    return this.attempt.key;
  }

  finish(committed: boolean): void {
    this.inFlight = false;
    if (committed) this.attempt = null;
  }

  reset(): void {
    if (!this.inFlight) this.attempt = null;
  }
}

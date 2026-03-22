/**
 * Shared Infrastructure — "Không-thời gian" mà modules sống trên.
 * Console output helpers, không chứa business logic.
 */
export function printHeader(title: string): void {
  const line = '═'.repeat(60);
  console.log();
  console.log(line);
  console.log(`  🌌 ${title}`);
  console.log(line);
}

export function printResult(module: string, command: string, args: string[], result: string): void {
  console.log(`  ▸ ${module} ${command} ${args.join(' ')}`);
  console.log(`    → ${result}`);
}

export function printBenchmark(label: string, iterations: number, elapsedMs: number): void {
  const opsPerSec = Math.round(iterations / (elapsedMs / 1000));
  console.log(`  ⚡ ${label}: ${iterations.toLocaleString()} ops in ${elapsedMs.toFixed(1)}ms (${opsPerSec.toLocaleString()} ops/sec)`);
}

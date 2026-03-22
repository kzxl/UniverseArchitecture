// ═══════════════════════════════════════════════════════════
//  🌌 Universe Architecture — TypeScript Demo
//  Module tự đăng ký → Registry dispatch → kết quả
// ═══════════════════════════════════════════════════════════

import { ModuleRegistry } from './core/ModuleRegistry';
import type { IModule } from './core/IModule';
import { CalculatorModule } from './modules/calculator/CalculatorModule';
import { GreeterModule } from './modules/greeter/GreeterModule';
import { printHeader, printResult } from './shared/ConsoleHelper';

const registry = new ModuleRegistry();

// ── Register modules (thêm module mới = thêm 1 dòng) ──
registry.register(new CalculatorModule());
registry.register(new GreeterModule());

// ══════════════════ Info ══════════════════
printHeader('Universe Architecture — TypeScript');

console.log(`\n  📦 Registered modules: ${registry.count}`);
for (const [name, mod] of registry.getAll()) {
  console.log(`     • ${name} — ${mod.description} [${mod.commands.join(', ')}]`);
}

// ══════════════════ Demo ══════════════════
printHeader('Demo Commands');

const demos: [string, string, string[]][] = [
  ['calculator', 'add', ['10', '25']],
  ['calculator', 'sub', ['100', '37']],
  ['calculator', 'mul', ['7', '8']],
  ['calculator', 'div', ['22', '7']],
  ['greeter', 'hello', ['Universe']],
  ['greeter', 'goodbye', ['Developer']],
];

for (const [mod, cmd, args] of demos) {
  const result = registry.dispatch(mod, cmd, args);
  printResult(mod, cmd, args, result);
}

// ══════════════════════════════════════════════════════════
//  ⚡ DETAILED BENCHMARK
// ══════════════════════════════════════════════════════════
printHeader('Detailed Performance Benchmark');

const WARMUP = 10_000;
const BENCH = 1_000_000;
const LATENCY_SAMPLES = 10_000;

// ── Phase 1: Warmup (V8 JIT) ──
console.log('\n  🔥 Phase 1: Warmup (V8 JIT)...');
for (let i = 0; i < WARMUP; i++) {
  registry.dispatch('calculator', 'add', ['1', '2']);
  registry.dispatch('greeter', 'hello', ['World']);
}
console.log(`     ${WARMUP.toLocaleString()} warmup iterations completed`);

// ── Phase 2: Throughput ──
console.log('\n  📊 Phase 2: Throughput (1M iterations each)');
console.log('  ┌────────────────────────────────┬────────────┬──────────────┐');
console.log('  │ Scenario                       │ Time (ms)  │ Ops/sec      │');
console.log('  ├────────────────────────────────┼────────────┼──────────────┤');

function runThroughput(label: string, fn: () => void, iterations: number): void {
  const start = performance.now();
  for (let i = 0; i < iterations; i++) fn();
  const elapsed = performance.now() - start;
  const opsPerSec = Math.round(iterations / (elapsed / 1000));
  console.log(`  │ ${label.padEnd(30)} │ ${elapsed.toFixed(1).padStart(10)} │ ${opsPerSec.toLocaleString().padStart(12)} │`);
}

runThroughput('calculator add 1 2', () => registry.dispatch('calculator', 'add', ['1', '2']), BENCH);
runThroughput('calculator mul 7 8', () => registry.dispatch('calculator', 'mul', ['7', '8']), BENCH);
runThroughput('calculator div 22 7', () => registry.dispatch('calculator', 'div', ['22', '7']), BENCH);
runThroughput('greeter hello World', () => registry.dispatch('greeter', 'hello', ['World']), BENCH);
runThroughput('greeter goodbye Dev', () => registry.dispatch('greeter', 'goodbye', ['Dev']), BENCH);

console.log('  └────────────────────────────────┴────────────┴──────────────┘');

// ── Phase 3: Latency Distribution ──
console.log(`\n  📈 Phase 3: Latency Distribution (${LATENCY_SAMPLES.toLocaleString()} samples)`);

function measureLatencies(fn: () => void, samples: number): number[] {
  const latencies: number[] = [];
  for (let i = 0; i < samples; i++) {
    const start = performance.now();
    fn();
    latencies.push((performance.now() - start) * 1_000_000); // ns
  }
  latencies.sort((a, b) => a - b);
  return latencies;
}

function printLatencyRow(label: string, sorted: number[]): void {
  const n = sorted.length;
  const min = sorted[0];
  const avg = sorted.reduce((a, b) => a + b, 0) / n;
  const p50 = sorted[Math.floor(n * 0.50)];
  const p95 = sorted[Math.floor(n * 0.95)];
  const p99 = sorted[Math.floor(n * 0.99)];
  console.log(`  │ ${label.padEnd(18)} │ ${min.toFixed(0).padStart(8)} │ ${avg.toFixed(0).padStart(8)} │ ${p50.toFixed(0).padStart(8)} │ ${p95.toFixed(0).padStart(8)} │ ${p99.toFixed(0).padStart(8)} │`);
}

const calcLat = measureLatencies(() => registry.dispatch('calculator', 'add', ['1', '2']), LATENCY_SAMPLES);
const greetLat = measureLatencies(() => registry.dispatch('greeter', 'hello', ['World']), LATENCY_SAMPLES);

console.log('  ┌────────────────────┬──────────┬──────────┬──────────┬──────────┬──────────┐');
console.log('  │ Scenario           │ Min (ns) │ Avg (ns) │ P50 (ns) │ P95 (ns) │ P99 (ns) │');
console.log('  ├────────────────────┼──────────┼──────────┼──────────┼──────────┼──────────┤');
printLatencyRow('calculator add', calcLat);
printLatencyRow('greeter hello', greetLat);
console.log('  └────────────────────┴──────────┴──────────┴──────────┴──────────┴──────────┘');

// ── Phase 4: Scalability ──
console.log('\n  🔬 Phase 4: Registry Scalability');
console.log('  ┌──────────────┬──────────────┬──────────────┐');
console.log('  │ # Modules    │ Dispatch/sec │ Overhead     │');
console.log('  ├──────────────┼──────────────┼──────────────┤');

class DummyModule implements IModule {
  readonly name: string;
  readonly description = 'Dummy';
  readonly commands = ['noop'] as const;
  constructor(id: string) { this.name = id; }
  execute(): string { return 'ok'; }
}

function benchOps(reg: ModuleRegistry, iterations: number): number {
  const start = performance.now();
  for (let i = 0; i < iterations; i++) reg.dispatch('calculator', 'add', ['1', '2']);
  return Math.round(iterations / ((performance.now() - start) / 1000));
}

const baseOps = benchOps(registry, BENCH / 10);

for (const n of [10, 40, 70, 100]) {
  const scaled = new ModuleRegistry();
  scaled.register(new CalculatorModule());
  for (let j = 1; j < n; j++) scaled.register(new DummyModule(`dummy_${j}`));

  const ops = benchOps(scaled, BENCH / 10);
  const overhead = (baseOps / ops - 1) * 100;
  console.log(`  │ ${String(n).padEnd(12)} │ ${ops.toLocaleString().padStart(12)} │ ${overhead > 0.5 ? `+${overhead.toFixed(1)}%` : 'baseline'}${' '.repeat(Math.max(0, 12 - (overhead > 0.5 ? `+${overhead.toFixed(1)}%` : 'baseline').length))} │`);
}

console.log('  └──────────────┴──────────────┴──────────────┘');

console.log();
console.log('  ✅ All benchmarks completed!');
console.log();

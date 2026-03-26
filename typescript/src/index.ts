// ═══════════════════════════════════════════════════════════
//  🌌 Universe Architecture — TypeScript Demo
//  Full demo: Registry + EventBus + Middleware + Lifecycle
// ═══════════════════════════════════════════════════════════

import { ModuleRegistry } from './core/ModuleRegistry';
import type { IModule } from './core/IModule';
import { CalculatorModule } from './modules/calculator/CalculatorModule';
import { GreeterModule } from './modules/greeter/GreeterModule';
import { NotifierModule } from './modules/notifier/NotifierModule';
import { LoggingMiddleware, TimingMiddleware, ErrorHandlingMiddleware } from './core/Middleware';
import { EventBus } from './core/EventBus';
import { printHeader, printResult } from './shared/ConsoleHelper';

const registry = new ModuleRegistry();

// ── Middleware pipeline (Gravity) ──
const loggingMw = new LoggingMiddleware();
registry.addMiddleware(new ErrorHandlingMiddleware());
registry.addMiddleware(new TimingMiddleware());
registry.addMiddleware(loggingMw);

// ── Register modules ──
registry.register(new CalculatorModule());
registry.register(new GreeterModule());

// Register NotifierModule with lifecycle
const notifierMod = new NotifierModule(registry.eventBus);
await registry.registerAsync(notifierMod);

// ══════════════════ Info ══════════════════
printHeader('Universe Architecture — TypeScript');

console.log(`\n  📦 Registered modules: ${registry.count}`);
console.log(`  🔗 Middleware pipeline: ${registry.middlewareCount} handlers`);
console.log(`  📡 EventBus: ${registry.eventBus.typeCount} event types, ${registry.eventBus.handlerCount} handlers`);

for (const [name, mod] of registry.getAll()) {
  console.log(`     • ${name} — ${mod.description} [${mod.commands.join(', ')}]`);
}

// ══════════════════ Demo ══════════════════
printHeader('Demo Commands (via Middleware Pipeline)');

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

  // Publish events for EventBus demo
  if (mod === 'calculator') {
    registry.eventBus.publish('CalculationPerformed', {
      operation: `${cmd} ${args.join(' ')}`,
      result,
    });
  } else if (mod === 'greeter') {
    registry.eventBus.publish('Greeting', {
      name: args[0] ?? 'World',
      message: result,
    });
  }
}

// ══════════════════ EventBus Demo ══════════════════
printHeader('EventBus — Indirect Communication');

console.log('\n  📡 Notifier received events from other modules:');
console.log(registry.dispatch('notifier', 'history', []));
console.log(`\n  ${registry.dispatch('notifier', 'count', [])}`);

// ══════════════════ Middleware Logs ══════════════════
printHeader('Middleware Pipeline — Gravity Logs');

console.log(`\n  📝 Logging middleware captured ${loggingMw.logs.length} dispatch(es):`);
for (const log of loggingMw.logs.slice(0, 5)) {
  console.log(`     ${log}`);
}
if (loggingMw.logs.length > 5) {
  console.log(`     ... and ${loggingMw.logs.length - 5} more`);
}

// ══════════════════ Lifecycle Demo ══════════════════
printHeader('Module Lifecycle — Star Lifecycle');

console.log('\n  🌟 Shutting down all modules with lifecycle hooks...');
await registry.shutdown();
console.log('  ✅ All lifecycle modules shut down gracefully.');

// ══════════════════════════════════════════════════════════
//  ⚡ DETAILED BENCHMARK
// ══════════════════════════════════════════════════════════
printHeader('Detailed Performance Benchmark');

const WARMUP = 10_000;
const BENCH = 1_000_000;
const LATENCY_SAMPLES = 10_000;

// Fresh registry without middleware for benchmark
const benchRegistry = new ModuleRegistry();
benchRegistry.register(new CalculatorModule());
benchRegistry.register(new GreeterModule());

// ── Phase 1: Warmup (V8 JIT) ──
console.log('\n  🔥 Phase 1: Warmup (V8 JIT)...');
for (let i = 0; i < WARMUP; i++) {
  benchRegistry.dispatch('calculator', 'add', ['1', '2']);
  benchRegistry.dispatch('greeter', 'hello', ['World']);
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

runThroughput('calculator add 1 2', () => benchRegistry.dispatch('calculator', 'add', ['1', '2']), BENCH);
runThroughput('calculator mul 7 8', () => benchRegistry.dispatch('calculator', 'mul', ['7', '8']), BENCH);
runThroughput('calculator div 22 7', () => benchRegistry.dispatch('calculator', 'div', ['22', '7']), BENCH);
runThroughput('greeter hello World', () => benchRegistry.dispatch('greeter', 'hello', ['World']), BENCH);
runThroughput('greeter goodbye Dev', () => benchRegistry.dispatch('greeter', 'goodbye', ['Dev']), BENCH);

console.log('  └────────────────────────────────┴────────────┴──────────────┘');

// ── Phase 2b: Throughput with Middleware ──
console.log('\n  📊 Phase 2b: Throughput WITH Middleware Pipeline');

const mwRegistry = new ModuleRegistry();
mwRegistry.register(new CalculatorModule());
mwRegistry.addMiddleware(new ErrorHandlingMiddleware());
mwRegistry.addMiddleware(new TimingMiddleware());

for (let i = 0; i < WARMUP; i++) mwRegistry.dispatch('calculator', 'add', ['1', '2']);

console.log('  ┌────────────────────────────────┬────────────┬──────────────┐');
console.log('  │ Scenario                       │ Time (ms)  │ Ops/sec      │');
console.log('  ├────────────────────────────────┼────────────┼──────────────┤');
runThroughput('with middleware (2 mw)', () => mwRegistry.dispatch('calculator', 'add', ['1', '2']), BENCH);
console.log('  └────────────────────────────────┴────────────┴──────────────┘');

// ── Phase 3: Latency Distribution ──
console.log(`\n  📈 Phase 3: Latency Distribution (${LATENCY_SAMPLES.toLocaleString()} samples)`);

function measureLatencies(fn: () => void, samples: number): number[] {
  const latencies: number[] = [];
  for (let i = 0; i < samples; i++) {
    const start = performance.now();
    fn();
    latencies.push((performance.now() - start) * 1_000_000);
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

const calcLat = measureLatencies(() => benchRegistry.dispatch('calculator', 'add', ['1', '2']), LATENCY_SAMPLES);
const greetLat = measureLatencies(() => benchRegistry.dispatch('greeter', 'hello', ['World']), LATENCY_SAMPLES);

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

const baseOps = benchOps(benchRegistry, BENCH / 10);

for (const n of [10, 40, 70, 100]) {
  const scaled = new ModuleRegistry();
  scaled.register(new CalculatorModule());
  for (let j = 1; j < n; j++) scaled.register(new DummyModule(`dummy_${j}`));

  const ops = benchOps(scaled, BENCH / 10);
  const overhead = (baseOps / ops - 1) * 100;
  const label = overhead > 0.5 ? `+${overhead.toFixed(1)}%` : 'baseline';
  console.log(`  │ ${String(n).padEnd(12)} │ ${ops.toLocaleString().padStart(12)} │ ${label.padEnd(12)} │`);
}

console.log('  └──────────────┴──────────────┴──────────────┘');

// ── Phase 5: EventBus Throughput ──
console.log('\n  📡 Phase 5: EventBus Throughput');
const benchBus = new EventBus();
benchBus.subscribe<any>('BenchEvent', () => { });

for (let i = 0; i < WARMUP; i++) benchBus.publish('BenchEvent', { v: 1 });

console.log('  ┌────────────────────────────────┬────────────┬──────────────┐');
console.log('  │ Scenario                       │ Time (ms)  │ Ops/sec      │');
console.log('  ├────────────────────────────────┼────────────┼──────────────┤');

runThroughput('eventbus publish (1 sub)', () => benchBus.publish('BenchEvent', { v: 1 }), BENCH);

console.log('  └────────────────────────────────┴────────────┴──────────────┘');

console.log();
console.log('  ✅ All benchmarks completed!');
console.log();

// ═══════════════════════════════════════════════════════════
//  🌌 Universe Architecture — TypeScript Demo
//  Module tự đăng ký → Registry dispatch → kết quả
// ═══════════════════════════════════════════════════════════

import { ModuleRegistry } from './core/ModuleRegistry';
import { CalculatorModule } from './modules/calculator/CalculatorModule';
import { GreeterModule } from './modules/greeter/GreeterModule';
import { printHeader, printResult, printBenchmark } from './shared/ConsoleHelper';

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

// ══════════════════ Benchmark ══════════════════
printHeader('Performance Benchmark');

const iterations = 1_000_000;

let start = performance.now();
for (let i = 0; i < iterations; i++) {
  registry.dispatch('calculator', 'add', ['1', '2']);
}
let elapsed = performance.now() - start;
printBenchmark('Registry Dispatch (calculator add 1 2)', iterations, elapsed);

start = performance.now();
for (let i = 0; i < iterations; i++) {
  registry.dispatch('greeter', 'hello', ['World']);
}
elapsed = performance.now() - start;
printBenchmark('Registry Dispatch (greeter hello World)', iterations, elapsed);

console.log();
console.log('  ✅ All demos completed successfully!');
console.log();

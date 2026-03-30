/**
 * Conformance Tests — Behavioral parity checks aligned with conformance/spec.yaml.
 * Run: npx tsx src/conformance/conformance.test.ts
 */
import { ModuleRegistry } from '../core/ModuleRegistry';
import { EventBus } from '../core/EventBus';
import { CalculatorModule } from '../modules/calculator/CalculatorModule';
import { GreeterModule } from '../modules/greeter/GreeterModule';
import { InventoryModule } from '../modules/inventory/InventoryModule';
import { SalesModule } from '../modules/sales/SalesModule';

let passed = 0;
let failed = 0;
const failures: string[] = [];

function assert(condition: boolean, message: string): void {
  if (condition) {
    passed++;
    console.log(`  ✅ ${message}`);
  } else {
    failed++;
    failures.push(message);
    console.log(`  ❌ ${message}`);
  }
}

function assertContains(actual: string, expected: string, testName: string): void {
  assert(actual.includes(expected), testName);
}

function assertThrows(fn: () => void, testName: string): void {
  try {
    fn();
    assert(false, testName + ' (expected throw, got none)');
  } catch {
    assert(true, testName);
  }
}

function createRegistry(): ModuleRegistry {
  const r = new ModuleRegistry();
  r.register(new CalculatorModule());
  r.register(new GreeterModule());
  return r;
}

console.log('\n🌌 Universe Architecture — TypeScript Conformance Tests\n');

// ═══════════════ Registry Core ═══════════════
console.log('Registry Core:');

assertContains(createRegistry().dispatch('calculator', 'add', ['10', '25']), '10 + 25 = 35', 'Calculator Add');
assertContains(createRegistry().dispatch('calculator', 'sub', ['100', '37']), '100 - 37 = 63', 'Calculator Sub');
assertContains(createRegistry().dispatch('calculator', 'mul', ['7', '8']), '7 * 8 = 56', 'Calculator Mul');
assertContains(createRegistry().dispatch('calculator', 'div', ['22', '7']), '22 / 7', 'Calculator Div');
assertContains(createRegistry().dispatch('calculator', 'div', ['10', '0']).toLowerCase(), 'division by zero', 'Calculator DivByZero');
assertContains(createRegistry().dispatch('greeter', 'hello', ['Universe']), 'Universe', 'Greeter Hello');

assertThrows(() => {
  const r = new ModuleRegistry();
  r.register(new CalculatorModule());
  r.register(new CalculatorModule());
}, 'Duplicate Registration Throws');

assertContains(createRegistry().dispatch('CALCULATOR', 'add', ['1', '2']), '1 + 2 = 3', 'Case-Insensitive Lookup');

assertThrows(() => createRegistry().dispatch('nonexistent', 'x', []), 'Unknown Module Throws');

// ═══════════════ EventBus ═══════════════
console.log('\nEventBus:');

(() => {
  const bus = new EventBus();
  let received = '';
  bus.subscribe<string>('TestEvent', (e) => received = e);
  bus.publish('TestEvent', 'hello');
  assert(received === 'hello', 'EventBus Publish/Subscribe');
})();

(() => {
  const bus = new EventBus();
  let count = 0;
  const handler = (_: string) => { count++; };
  bus.subscribe<string>('TestEvent', handler);
  bus.publish('TestEvent', 'first');
  assert(count === 1, 'EventBus Subscribe Received');
  bus.unsubscribe<string>('TestEvent', handler);
  bus.publish('TestEvent', 'second');
  assert(count === 1, 'EventBus Unsubscribe Stops Delivery');
})();

// ═══════════════ Enterprise Scenario ═══════════════
console.log('\nEnterprise Scenario:');

(() => {
  const bus = new EventBus();
  const r = new ModuleRegistry();
  r.register(new SalesModule(bus));
  const result = r.dispatch('sales', 'create-order', ['PROD-001', '5']);
  assertContains(result, 'ORD-', 'Sales Create Order');
})();

(() => {
  const bus = new EventBus();
  const r = new ModuleRegistry();
  r.register(new InventoryModule(bus));
  assertContains(r.dispatch('inventory', 'check', ['PROD-001']), 'PROD-001', 'Inventory Check Stock');
})();

(() => {
  const bus = new EventBus();
  const r = new ModuleRegistry();
  r.register(new InventoryModule(bus));
  assertContains(r.dispatch('inventory', 'deduct', ['PROD-001', '999']), 'Insufficient', 'Inventory Deduct Insufficient');
})();

(() => {
  const bus = new EventBus();
  const r = new ModuleRegistry();
  r.register(new InventoryModule(bus));
  assertContains(r.dispatch('inventory', 'list', []), 'PROD-001', 'Inventory List Products');
})();

// ═══════════════ Summary ═══════════════
console.log(`\n${'═'.repeat(50)}`);
console.log(`  Total: ${passed + failed}, Passed: ${passed}, Failed: ${failed}`);
if (failures.length > 0) {
  console.log('  Failures:');
  failures.forEach(f => console.log(`    ❌ ${f}`));
  process.exit(1);
} else {
  console.log('  ✅ All conformance tests passed!');
}

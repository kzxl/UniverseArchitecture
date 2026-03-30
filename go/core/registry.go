package core

import (
	"fmt"
	"strings"
)

// Registry — Trung tâm đăng ký module.
// Upgraded: Tích hợp EventBus, Middleware pipeline, Lifecycle hooks.
type Registry struct {
	modules     map[string]Module
	middlewares []Middleware
	EventBus    *EventBus
}

// NewRegistry khởi tạo registry rỗng.
func NewRegistry() *Registry {
	return &Registry{
		modules:  make(map[string]Module),
		EventBus: NewEventBus(),
	}
}

// Register đăng ký 1 module. Trùng tên → panic.
func (r *Registry) Register(m Module) {
	name := strings.ToLower(m.Name())
	if _, exists := r.modules[name]; exists {
		panic(fmt.Sprintf("module '%s' already registered", name))
	}
	r.modules[name] = m
}

// RegisterWithLifecycle đăng ký module với lifecycle hooks.
func (r *Registry) RegisterWithLifecycle(m Module) error {
	if lc, ok := m.(ModuleLifecycle); ok {
		if err := lc.OnInitializing(); err != nil {
			return fmt.Errorf("OnInitializing failed for '%s': %w", m.Name(), err)
		}
	}

	r.Register(m)

	if lc, ok := m.(ModuleLifecycle); ok {
		if err := lc.OnInitialized(); err != nil {
			return fmt.Errorf("OnInitialized failed for '%s': %w", m.Name(), err)
		}
	}
	return nil
}

// AddMiddleware thêm middleware vào pipeline. FIFO order.
func (r *Registry) AddMiddleware(mw Middleware) {
	r.middlewares = append(r.middlewares, mw)
}

// Dispatch chuyển command tới module, qua middleware pipeline.
// Pipeline executes module at end of chain — middleware can read ctx.Result after next().
func (r *Registry) Dispatch(moduleName, command string, args []string) string {
	m, ok := r.modules[strings.ToLower(moduleName)]
	if !ok {
		keys := make([]string, 0, len(r.modules))
		for k := range r.modules {
			keys = append(keys, k)
		}
		return fmt.Sprintf("module '%s' not found. Available: %s", moduleName, strings.Join(keys, ", "))
	}

	if len(r.middlewares) == 0 {
		return m.Execute(command, args)
	}

	ctx := NewContext(moduleName, command, args)
	r.runPipeline(ctx, m)
	return ctx.Result
}

func (r *Registry) runPipeline(ctx *ModuleContext, m Module) {
	index := -1
	var next func()
	next = func() {
		index++
		if index < len(r.middlewares) {
			r.middlewares[index].Invoke(ctx, next)
		} else {
			// End of pipeline → execute module. ctx.Result available for middleware after next().
			ctx.Result = m.Execute(ctx.Command, ctx.Args)
		}
	}
	next()
}

// Shutdown tất cả modules có lifecycle.
func (r *Registry) Shutdown() error {
	for _, m := range r.modules {
		if lc, ok := m.(ModuleLifecycle); ok {
			if err := lc.OnShuttingDown(); err != nil {
				return err
			}
			if err := lc.OnShutdown(); err != nil {
				return err
			}
		}
	}
	return nil
}

// GetAll trả về tất cả modules.
func (r *Registry) GetAll() map[string]Module {
	return r.modules
}

// Count trả về số lượng modules.
func (r *Registry) Count() int {
	return len(r.modules)
}

// MiddlewareCount trả về số middleware.
func (r *Registry) MiddlewareCount() int {
	return len(r.middlewares)
}

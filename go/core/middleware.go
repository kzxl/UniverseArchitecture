package core

// ModuleContext — Ngữ cảnh cho mỗi dispatch, đi qua middleware pipeline.
type ModuleContext struct {
	ModuleName string
	Command    string
	Args       []string
	Result     string
	Items      map[string]interface{}
}

// NewContext tạo context mới.
func NewContext(moduleName, command string, args []string) *ModuleContext {
	return &ModuleContext{
		ModuleName: moduleName,
		Command:    command,
		Args:       args,
		Items:      make(map[string]interface{}),
	}
}

// Middleware — Lực hấp dẫn (Gravity). Principle #7.
// Chain of Responsibility pattern.
type Middleware interface {
	Invoke(ctx *ModuleContext, next func())
}

// MiddlewareFunc adapter — function as Middleware.
type MiddlewareFunc func(ctx *ModuleContext, next func())

func (f MiddlewareFunc) Invoke(ctx *ModuleContext, next func()) {
	f(ctx, next)
}

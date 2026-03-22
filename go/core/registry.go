package core

import (
	"fmt"
	"strings"
)

// Registry — Trung tâm đăng ký module.
// Module tự đăng ký qua init(). Core không biết chi tiết module.
// Giống vũ trụ tự tổ chức — thiên hà hình thành tự nhiên.
type Registry struct {
	modules map[string]Module
}

// NewRegistry khởi tạo registry rỗng.
func NewRegistry() *Registry {
	return &Registry{modules: make(map[string]Module)}
}

// Register đăng ký 1 module. Trùng tên → panic.
func (r *Registry) Register(m Module) {
	name := strings.ToLower(m.Name())
	if _, exists := r.modules[name]; exists {
		panic(fmt.Sprintf("module '%s' already registered", name))
	}
	r.modules[name] = m
}

// Dispatch chuyển command tới module phù hợp.
func (r *Registry) Dispatch(moduleName, command string, args []string) string {
	m, ok := r.modules[strings.ToLower(moduleName)]
	if !ok {
		keys := make([]string, 0, len(r.modules))
		for k := range r.modules {
			keys = append(keys, k)
		}
		return fmt.Sprintf("module '%s' not found. Available: %s", moduleName, strings.Join(keys, ", "))
	}
	return m.Execute(command, args)
}

// GetAll trả về tất cả modules.
func (r *Registry) GetAll() map[string]Module {
	return r.modules
}

// Count trả về số lượng modules.
func (r *Registry) Count() int {
	return len(r.modules)
}

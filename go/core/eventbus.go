package core

import (
	"reflect"
	"sync"
)

// EventBus — Sóng hấp dẫn (Gravitational Waves).
// Giao tiếp gián tiếp giữa modules. Principle #5.
type EventBus struct {
	mu       sync.RWMutex
	handlers map[reflect.Type][]interface{}
}

// NewEventBus khởi tạo EventBus.
func NewEventBus() *EventBus {
	return &EventBus{handlers: make(map[reflect.Type][]interface{})}
}

// Subscribe đăng ký handler cho event type T.
// handler phải là func(T).
func Subscribe[T any](bus *EventBus, handler func(T)) {
	bus.mu.Lock()
	defer bus.mu.Unlock()

	t := reflect.TypeFor[T]()
	bus.handlers[t] = append(bus.handlers[t], handler)
}

// Publish phát event tới tất cả subscribers.
func Publish[T any](bus *EventBus, event T) {
	bus.mu.RLock()
	defer bus.mu.RUnlock()

	t := reflect.TypeFor[T]()
	for _, h := range bus.handlers[t] {
		h.(func(T))(event)
	}
}

// TypeCount trả về số event types có subscribers.
func (bus *EventBus) TypeCount() int {
	bus.mu.RLock()
	defer bus.mu.RUnlock()
	return len(bus.handlers)
}

// HandlerCount trả về tổng handlers.
func (bus *EventBus) HandlerCount() int {
	bus.mu.RLock()
	defer bus.mu.RUnlock()
	count := 0
	for _, h := range bus.handlers {
		count += len(h)
	}
	return count
}

package core

// Module — Quy luật vật lý (Physics Law).
// Interface bất biến mà MỌI module phải tuân thủ.
// Giống như lực hấp dẫn — mọi vật thể đều bị ảnh hưởng.
type Module interface {
	// Name trả về tên duy nhất của module
	Name() string
	// Description trả về mô tả ngắn
	Description() string
	// Commands trả về danh sách commands hỗ trợ
	Commands() []string
	// Execute chạy command với args, trả về kết quả
	Execute(command string, args []string) string
}

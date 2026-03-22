/**
 * Quy luật vật lý (Physics Law) — Interface bất biến mà MỌI module phải tuân thủ.
 * Giống như lực hấp dẫn tác động lên mọi vật thể trong vũ trụ,
 * IModule định nghĩa "hợp đồng" mà mọi module phải ký.
 */
export interface IModule {
  /** Tên duy nhất của module */
  readonly name: string;
  /** Mô tả ngắn */
  readonly description: string;
  /** Danh sách commands hỗ trợ */
  readonly commands: readonly string[];
  /** Thực thi command với args, trả về kết quả */
  execute(command: string, args: string[]): string;
}

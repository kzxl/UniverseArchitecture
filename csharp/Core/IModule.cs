namespace UniverseDemo.Core;

/// <summary>
/// Quy luật vật lý (Physics Law) — Interface bất biến mà MỌI module phải tuân thủ.
/// Giống như lực hấp dẫn tác động lên mọi vật thể trong vũ trụ,
/// IModule định nghĩa "hợp đồng" mà mọi module phải ký.
/// </summary>
public interface IModule
{
    /// <summary>Tên duy nhất của module (ví dụ: "calculator", "greeter")</summary>
    string Name { get; }

    /// <summary>Mô tả ngắn về module</summary>
    string Description { get; }

    /// <summary>Danh sách commands mà module hỗ trợ</summary>
    IReadOnlyList<string> Commands { get; }

    /// <summary>
    /// Thực thi một command với tham số.
    /// Trả về kết quả dạng string.
    /// </summary>
    string Execute(string command, string[] args);
}

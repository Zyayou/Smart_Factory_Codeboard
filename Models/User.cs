using System.ComponentModel.DataAnnotations;

namespace SmartFactory_MinProject.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "아이디를 입력하세요.")]
        public string AccountId { get; set; } = null!;
        [Required(ErrorMessage = "비밀번호를 입력하세요.")]
        public string AccountPassword { get; set; } = null!;

        [Required(ErrorMessage = "이름을 입력하세요.")]
        public string Name { get; set; } = null!;
    }
}

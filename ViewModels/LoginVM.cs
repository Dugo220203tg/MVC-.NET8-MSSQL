using System.ComponentModel.DataAnnotations;

namespace TDProjectMVC.ViewModels
{
	public class LoginVM
	{
		[Display(Name = "User Name")]
		[Required(ErrorMessage ="*")]
		[MaxLength(20, ErrorMessage = "Max 20 keys")]
		public string UserName {  get; set; }
		[Display(Name = "User Name")]
		[Required(ErrorMessage = "*")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}

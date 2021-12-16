using System;
using System.Linq;

namespace UnoExtensions.ViewModels;

public record Credentials
{
	public string UserName { get; set; }

	public string Password { get; set; }
}

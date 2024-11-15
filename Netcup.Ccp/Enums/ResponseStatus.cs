using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;

namespace AMWD.Net.Api.Netcup.Ccp
{
	/// <summary>
	/// netcup response status.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ResponseStatus
	{
		/// <summary>
		/// The requested action is pending.
		/// </summary>
		[EnumMember(Value = "pending")]
		Pending = 1,

		/// <summary>
		/// The requested action has been started.
		/// </summary>
		[EnumMember(Value = "started")]
		Started = 2,

		/// <summary>
		/// The requested action has succeeded.
		/// </summary>
		[EnumMember(Value = "success")]
		Success = 3,

		/// <summary>
		/// The requested action has warnings.
		/// </summary>
		[EnumMember(Value = "warning")]
		Warning = 4,

		/// <summary>
		/// The requested action has failed.
		/// </summary>
		[EnumMember(Value = "error")]
		Error = 5,
	}
}

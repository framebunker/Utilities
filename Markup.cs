using System;


// Markup used in connection with code analysis and other tooling
namespace framebunker
{
	[
		AttributeUsage (
			AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter,
			AllowMultiple = false,
			Inherited = false
		)
	]
	public class NotNullAttribute : Attribute
	{}


	[
		AttributeUsage (
			AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter,
			AllowMultiple = false,
			Inherited = false
		)
	]
	public class CanBeNullAttribute : Attribute
	{}
}

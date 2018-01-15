using System;
using System.Collections.Generic;

namespace HeadphoneVolumeToggler
{
	public static class GenericUtil
	{
		public static T FindMaxValue<T>(this IEnumerable<T> list, Func<T, DateTimeOffset> projection) where T : class
		{
			T max = null;
			var maxValue = DateTimeOffset.MinValue;
			foreach (var item in list)
			{
				var value = projection(item);
				if (value > maxValue)
				{
					maxValue = value;
					max = item;
				}
			}

			return max;
		}
	}
}
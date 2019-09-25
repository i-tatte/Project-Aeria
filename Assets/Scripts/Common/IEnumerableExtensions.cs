using System;
using System.Collections.Generic;
using System.Linq;

public static class IEnumerableExtensions {
    /// <summary>
    /// 目的の値に最も近い値を返します
    /// </summary>
    public static float Nearest (
        this IEnumerable<float> self,
        float target
    ) {
        var min = self.Min (c => Math.Abs (c - target));
        return self.First (c => Math.Abs (c - target) == min);
    }
}
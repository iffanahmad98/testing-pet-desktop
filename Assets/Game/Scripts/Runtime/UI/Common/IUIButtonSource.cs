using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Contract for any UI component that wants to expose its buttons
/// to the global tutorial/UI button cache.
/// </summary>
public interface IUIButtonSource
{
    /// <summary>
    /// Add this component's relevant buttons into the provided list.
    /// Implementation should avoid adding nulls and duplicates.
    /// </summary>
    void CollectButtons(List<Button> target);
}

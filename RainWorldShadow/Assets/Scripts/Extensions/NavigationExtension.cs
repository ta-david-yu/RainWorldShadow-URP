using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum NavigationDirection
{
    Up,
    Down,
    Left,
    Right
}

public static class NavigationExtension
{
    public static Navigation GetModifiedNavigation(this Navigation source, NavigationDirection direction, Selectable item)
    {
        switch (direction)
        {
            case NavigationDirection.Down:
                source.selectOnDown = item;
                break;
            case NavigationDirection.Left:
                source.selectOnLeft = item;
                break;
            case NavigationDirection.Right:
                source.selectOnRight = item;
                break;
            case NavigationDirection.Up:
                source.selectOnUp = item;
                break;
        }
        return source;
    }
}

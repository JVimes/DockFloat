**DockFloat** provides a simple way to dock and float UI elements. You could use it for toolbars. Each has a "home" dock that it goes back to when its window is closed.

To make an element floatable, wrap it in a `Dock` (see working demo in source code):

```xml
<!-- xmlns:df="clr-namespace:DockFloat;assembly=DockFloat" -->
<df:Dock>
    <TextBlock Text="This part of the UI can be floated and docked."/>
</df:Dock>
```
DockFloat provides a way to pop WPF elements into floating windows.

![](demo.gif)

To use it, install via NuGet then wrap an element in a Dock:  

```xml
<df:Dock xmlns:df="clr-namespace:DockFloat;assembly=DockFloat">
    <TextBlock Text="This can be floated."/>
</df:Dock>
```

* `Dock` has some configuration properties.
* There is a working demo in the source code.
* Implicit styles outside the Dock have no affect in the floated window. Styles with x:Key do.
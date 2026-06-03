# Hero GLB Model Naming

Use `hero-{service-slug}-{short-subject}-{nn}.glb` for production hero variants.
Sample/demo files must use the `sample` base name.

Examples:

- `sample.glb`
- `hero-3d-printing-part-03.glb`
- `hero-cnc-machining-fixture-01.glb`
- `hero-3d-scanning-part-01.glb`

Add each file to `Maliev.Web.Client/Content/HeroModelCatalog.cs` so home and service-page heroes can choose the right model for the visitor intent.

using Logic;

namespace Visualizer
{
	public interface IOverlay
	{
		void Draw(Map map, IDrawer drawer);
	}
}
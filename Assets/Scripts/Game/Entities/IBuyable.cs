using Kit.Containers;

namespace Game
{
	public interface IBuyable
	{
		Bunch<Currency> Cost { get; }
		bool Buy();
		bool CanBuy { get; }
		bool IsBought { get; }
	}
}
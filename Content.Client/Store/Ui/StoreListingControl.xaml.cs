using Content.Client.GameTicking.Managers;
using Content.Shared.Store;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.Store.Ui;

// goob edit - fuck newstore
// do not touch unless you want to shoot yourself in the leg
[GenerateTypedNameReferences]
public sealed partial class StoreListingControl : Control
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private readonly ClientGameTicker _ticker;

    private readonly ListingData _data;

    private readonly bool _hasBalance;
    private readonly string _price;
    public StoreListingControl(ListingData data, string price, bool hasBalance, Texture? texture = null)
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);

        _ticker = _entity.System<ClientGameTicker>();

        _data = data;
        _hasBalance = hasBalance;
        _price = price;

        StoreItemName.Text = ListingLocalisationHelpers.GetLocalisedNameOrEntityName(_data, _prototype);
        StoreItemDescription.SetMessage(ListingLocalisationHelpers.GetLocalisedDescriptionOrEntityDescription(_data, _prototype));

        UpdateBuyButtonText();
        StoreItemBuyButton.Disabled = !CanBuy();

        StoreItemTexture.Texture = texture;
    }

    private bool CanBuy()
    {
        if (!_hasBalance)
            return false;

        var stationTime = _timing.CurTime.Subtract(_ticker.RoundStartTimeSpan);
        if (_data.RestockTime > stationTime)
            return false;

        return true;
    }

    private void UpdateBuyButtonText()
    {
        var stationTime = _timing.CurTime.Subtract(_ticker.RoundStartTimeSpan);
        if (_data.RestockTime > stationTime)
        {
            var timeLeftToBuy = stationTime - _data.RestockTime;
            StoreItemBuyButton.Text =  timeLeftToBuy.Duration().ToString(@"mm\:ss");
        }
        else
        {
            StoreItemBuyButton.Text = _price;
        }
    }

    private void UpdateName()
    {
        var name = ListingLocalisationHelpers.GetLocalisedNameOrEntityName(_data, _prototype);

        var stationTime = _timing.CurTime.Subtract(_ticker.RoundStartTimeSpan);
        if (_data.RestockTime > stationTime)
        {
            name += Loc.GetString("store-ui-button-out-of-stock");
        }

        StoreItemName.Text = name;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        UpdateBuyButtonText();
        UpdateName();
        StoreItemBuyButton.Disabled = !CanBuy();
    }
}


//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;
using SimpleJSON;

namespace cfg
{
public partial class Tables
{
    public StarForce.TbAircraft TbAircraft {get; }
    public StarForce.TbArmor TbArmor {get; }
    public StarForce.TbAsteroid TbAsteroid {get; }
    public StarForce.TbEntity TbEntity {get; }
    public StarForce.TbMusic TbMusic {get; }
    public StarForce.TbScene TbScene {get; }
    public StarForce.TbSound TbSound {get; }
    public StarForce.TbThruster TbThruster {get; }
    public StarForce.TbUIForm TbUIForm {get; }
    public StarForce.TbUISound TbUISound {get; }
    public StarForce.TbWeapon TbWeapon {get; }
    public Localization.TbL10N TbL10N {get; }

    public Tables(System.Func<string, JSONNode> loader)
    {
        TbAircraft = new StarForce.TbAircraft(loader("starforce_tbaircraft"));
        TbArmor = new StarForce.TbArmor(loader("starforce_tbarmor"));
        TbAsteroid = new StarForce.TbAsteroid(loader("starforce_tbasteroid"));
        TbEntity = new StarForce.TbEntity(loader("starforce_tbentity"));
        TbMusic = new StarForce.TbMusic(loader("starforce_tbmusic"));
        TbScene = new StarForce.TbScene(loader("starforce_tbscene"));
        TbSound = new StarForce.TbSound(loader("starforce_tbsound"));
        TbThruster = new StarForce.TbThruster(loader("starforce_tbthruster"));
        TbUIForm = new StarForce.TbUIForm(loader("starforce_tbuiform"));
        TbUISound = new StarForce.TbUISound(loader("starforce_tbuisound"));
        TbWeapon = new StarForce.TbWeapon(loader("starforce_tbweapon"));
        TbL10N = new Localization.TbL10N(loader("localization_tbl10n"));
        ResolveRef();
    }
    
    private void ResolveRef()
    {
        TbAircraft.ResolveRef(this);
        TbArmor.ResolveRef(this);
        TbAsteroid.ResolveRef(this);
        TbEntity.ResolveRef(this);
        TbMusic.ResolveRef(this);
        TbScene.ResolveRef(this);
        TbSound.ResolveRef(this);
        TbThruster.ResolveRef(this);
        TbUIForm.ResolveRef(this);
        TbUISound.ResolveRef(this);
        TbWeapon.ResolveRef(this);
        TbL10N.ResolveRef(this);
    }
}

}
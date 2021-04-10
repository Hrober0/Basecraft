using UnityEngine;
using System;

public class ImageLibrary : MonoBehaviour
{
    [Header("Platform image")]
    [SerializeField] private Sprite Warehouse = null;
    [SerializeField] private Sprite Warehouse2 = null;
    [SerializeField] private Sprite WoodCuter = null;
    [SerializeField] private Sprite Quarry = null;
    [SerializeField] private Sprite Planter = null;
    [SerializeField] private Sprite Smelter = null;
    [SerializeField] private Sprite Pulverizer = null;
    [SerializeField] private Sprite Pump = null;
    [SerializeField] private Sprite Farm = null;
    [SerializeField] private Sprite Junkyard = null;

    [SerializeField] private Sprite Balista = null;
    [SerializeField] private Sprite GunTurret = null;
    [SerializeField] private Sprite LaserTurret = null;
    [SerializeField] private Sprite RocketTurret = null;

    [SerializeField] private Sprite Wall0 = null;
    [SerializeField] private Sprite Wall1 = null;
    [SerializeField] private Sprite Wall2 = null;
    [SerializeField] private Sprite Wall3 = null;

    [SerializeField] private Sprite BasicCrafter = null;
    [SerializeField] private Sprite Crafter = null;
    [SerializeField] private Sprite ChemicalPlant = null;
    [SerializeField] private Sprite DronStation = null;

    [SerializeField] private Sprite Connector = null;
    //[SerializeField] private Sprite FastConnector = null;

    [SerializeField] private Sprite WindTurbine1 = null;
    //[SerializeField] private Sprite WindTurbine2 = null;
    [SerializeField] private Sprite TransmissionTower = null;
    [SerializeField] private Sprite CombustionGenerator = null;
    [SerializeField] private Sprite SteemGenerator = null;
    [SerializeField] private Sprite Battery1 = null;
    [SerializeField] private Sprite SolarPanel1 = null;

    [SerializeField] private Sprite ElectricSmelter = null;
    //[SerializeField] private Sprite Repairer = null;

    [SerializeField] private Sprite Launchpad = null;
    //[SerializeField] private Sprite BasicRequester = null;
    //[SerializeField] private Sprite SpaceRequester = null;

    [Header("Res image")]
    [SerializeField] private Sprite[] resIcons;

    public static ImageLibrary instance;
    private void Awake()
    {
        if (instance != null) { return; }
        instance = this;
        if (resIcons == null || resIcons.Length == 0) { SetResIcons(); }
    }

    public void SetResIcons()
    {
        Debug.Log("Set res sprites");
        int ResLenght = Enum.GetNames(typeof(Res)).Length;
        resIcons = new Sprite[ResLenght];
        for (int i = 0; i < ResLenght; i++)
        {
            Sprite tempsprite = Resources.Load<Sprite>(string.Format("ResIcons/{0}", (Res)i));
            if (tempsprite == null) { Debug.LogWarning("ERROR! Sprite: " + (Res)i + " not found"); }
            else { resIcons[i] = tempsprite; }
        }
        Resources.UnloadUnusedAssets();
    }

    public Sprite GetResImage(Res res)
    {
        if ((int)res >= resIcons.Length) { return null; }
        return resIcons[(int)res];
    }

    public Sprite GetObjImages(Obj obj)
    {
        switch (obj)
        {
            //Building
            case Obj.BuildingUnderDemolition: return null;
            case Obj.BuildingUnderConstruction: return null;

            case Obj.Warehouse1:        return Warehouse;
            case Obj.Warehouse2:        return Warehouse2;
            case Obj.Woodcuter:         return WoodCuter;
            case Obj.Quarry:            return Quarry;
            case Obj.Planter:           return Planter;
            case Obj.Smelter:           return Smelter;
            case Obj.Pulverizer:        return Pulverizer;
            case Obj.Pump:              return Pump;
            case Obj.Farm:              return Farm;
            case Obj.Junkyard:          return Junkyard;

            case Obj.Ballista:          return Balista;
            case Obj.GunTurret:         return GunTurret;
            case Obj.LaserTurret:       return LaserTurret;
            case Obj.RocketTurret:      return RocketTurret;

            case Obj.Wall0:             return Wall0;
            case Obj.Wall1:             return Wall1;
            case Obj.Wall2:             return Wall2;
            case Obj.Wall3:             return Wall3;

            case Obj.BasicCrafter:      return BasicCrafter;
            case Obj.Crafter:           return Crafter;
            case Obj.ChemicalPlant:     return ChemicalPlant;
            case Obj.DroneStation:      return DronStation;

            case Obj.Connector:         return Connector;
            //case Obj.FastConnector:   return FastConnector;

            case Obj.WindTurbine1:      return WindTurbine1;
            //case Obj.WindTurbine2:      return WindTurbine2;
            case Obj.TransmissionTower: return TransmissionTower;
            case Obj.CombustionGenerator:return CombustionGenerator;
            case Obj.SteamGenerator:    return SteemGenerator;
            case Obj.Battery:           return Battery1;
            case Obj.SolarPanel1:       return SolarPanel1;

            case Obj.ElectricSmelter:   return ElectricSmelter;
            //case Obj.Repairer:        return Repairer;

            case Obj.Launchpad:         return Launchpad;
            //case Obj.SpaceRequester:  return SpaceRequester;
        }

        if (TerrainManager.instance != null)
        {
            Sprite sprite = TerrainManager.instance.GetTerrainImages(obj);
            if (sprite != null) { return sprite; }
        }

        Debug.Log(obj + " don have sprite reference");
        return null;
    }
}
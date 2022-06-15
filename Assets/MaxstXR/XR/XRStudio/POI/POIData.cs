using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POIData
{
    public int id { get; set; }
    public string poi_uuid { get; set; }
    public string poi_name_ko { get; set; }
    public string poi_name_en { get; set; }
    public CategoryData category_info { get; set; }
    public int view_level { get; set; }
    public int floor { get; set; }
    public string tags { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }
    public float vps_x { get; set; }
    public float vps_y { get; set; }
    public float vps_z { get; set; }
    public string connect_poi_uuid { get; set; }

    public Vector3 GetVPSPosition()
    {
        return new Vector3(vps_x, vps_y, vps_z);
    }
}

public class CategoryData
{
    public int category_id { get; set; }
    public string category_name_ko { get; set; }
    public string category_name_en { get; set; }
    public string category_icon { get; set; }
    public int map_poi_category_joint_type_id { get; set; }
    public int map_poi_category_augment_type_id { get; set; }
    public bool is_joint_type { get; set; }
}
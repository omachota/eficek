use proj::Proj;

#[repr(C)]
pub struct Coordinate {
    pub latitude: f64,
    pub longitude: f64,
}

#[repr(C)]
pub struct UtmCoordinate {
    pub northing: f64,
    pub easting: f64,
    pub zone_number: i32,
}

// The caller is responsible for arrays to have the same length
// according to: https://www.reddit.com/r/rust/comments/kcetvf/c_interoppinvoke_passing_arrays/
// It is up to the caller to ensure that all coordinates are from a single zone
#[no_mangle]
pub unsafe extern "cdecl" fn ConvertArrayUnsafe(ptr_coord: *const Coordinate, ptr_utm: *mut UtmCoordinate, length: usize, zone: usize) {
    let coordinates: &[Coordinate] = unsafe { std::slice::from_raw_parts(ptr_coord, length) };
    let utm_coordinates: &mut [UtmCoordinate] = unsafe { std::slice::from_raw_parts_mut(ptr_utm, length) };

    convert_array(coordinates, utm_coordinates, zone);
}

#[no_mangle]
pub unsafe extern "cdecl" fn ConvertUnsafe(coordinate: Coordinate, zone: usize) -> UtmCoordinate{
    convert(&coordinate, zone)
}

pub fn get_proj(coordinate: &Coordinate, zone: usize) -> Proj {
    let north_or_south = if coordinate.latitude >= 0f64 { '6' } else { '7' };
    let from = "EPSG:4326"; // WGS84
    // let to = format!("EPSG:32{north_or_south}{zone}").as_str();
    Proj::new_known_crs(from, format!("EPSG:32{north_or_south}{zone}").as_str(), None).expect("failed to create projection")
}

pub fn convert_array(coordinates: &[Coordinate], utm_coordinates: &mut [UtmCoordinate], zone: usize) {
    let projection = get_proj(&coordinates[0], zone);

    // Longitude, Latitude / Easting, Northing
    for (i, coordinate) in coordinates.iter().enumerate()
    {
        (utm_coordinates[i].easting, utm_coordinates[i].northing) = projection.convert((coordinate.latitude, coordinate.longitude)).expect("failed to convert coordinate");
        utm_coordinates[i].zone_number = zone as i32;
    }
}

// Returning struct is simple
pub fn convert(coordinate: &Coordinate, zone: usize) -> UtmCoordinate {
    let projection = get_proj(coordinate, zone);

    // Longitude, Latitude / Easting, Northing
    let (easting, northing) = projection.convert((coordinate.latitude, coordinate.longitude)).expect("failed to convert coordinate");
    UtmCoordinate {
        northing,
        easting,
        zone_number: zone as i32,
    }
}

pub fn add(left: usize, right: usize) -> usize {
    left + right
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn it_works() {
        let result = add(2, 2);
        assert_eq!(result, 4);
    }
}

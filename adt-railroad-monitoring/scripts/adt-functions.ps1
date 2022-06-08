function New-TargetSensor {
    param (
        $dtname,
        $section,
        $station_number,
        $sensor_numbers
    )
    
    $station = "station_$($station_number)"

    az dt twin create `
        -n $dtname `
        -m 'dtmi:railmonitoring:station;1' `
        -t $station `
        -p ('{ \"number\": ' + $station_number + ', \"name\": \"station ' + $station_number + '\", \"type\": \"target\", \"sideMovement\": { \"$metadata\": {}, \"value1\": 10, \"value2\": 15 }, \"cant\": { \"$metadata\": {}, \"value1\": 8, \"value2\": 9 } }')

    az dt twin relationship create -n $dtname --relationship 'has_stations' --twin-id $section --target $station --relationship-id "$($section)_$($station)"

    #region station's sensors
    foreach ($number in $sensor_numbers) {

        $sensor = "sensor_$($number)"
        az dt twin create `
            -n $dtname `
            -m 'dtmi:railmonitoring:sensor:target;1' `
            -t $sensor `
            -p '{ \"northing\": 10, \"easting\": 20, \"elevation\": 30 }'

        az dt twin relationship create -n $dtname --relationship 'has_target_sensors' --twin-id $station --target $sensor --relationship-id "$($station)_$($sensor)"
    }
    #endregion
}

function New-TiltSensor {
    param (
        $dtname,
        $section,
        $station_number,
        $sensor_numbers
    )
    
    $station = "station_$($station_number)"

    az dt twin create `
        -n $dtname `
        -m 'dtmi:railmonitoring:station;1' `
        -t $station `
        -p ('{ \"number\": ' + $station_number + ', \"name\": \"station ' + $station_number + '\", \"type\": \"target\", \"sideMovement\": { \"$metadata\": {}, \"value1\": 10, \"value2\": 15 }, \"cant\": { \"$metadata\": {}, \"value1\": 8, \"value2\": 9 } }')

    az dt twin relationship create -n $dtname --relationship 'has_stations' --twin-id $section --target $station --relationship-id "$($section)_$($station)"

    #region station's sensors
    foreach ($number in $sensor_numbers) {

        $sensor = "sensor_$($number)"
        az dt twin create `
            -n $dtname `
            -m 'dtmi:railmonitoring:sensor:tilt;1' `
            -t $sensor `
            -p '{ \"tiltA\": 8, \"tiltB\": 12 }'

        az dt twin relationship create -n $dtname --relationship 'has_tilt_sensors' --twin-id $station --target $sensor --relationship-id "$($station)_$($sensor)"
    }
    #endregion
}

function New-TwistMeasure {
    param (
        $dtname,
        $section,
        $twist_number,
        $station_numbers
    )
    
    $twist = "twist_$($twist_number)"

    az dt twin create `
        -n $dtname `
        -m 'dtmi:railmonitoring:measurements:twist;3' `
        -t $twist `
        -p ('{ \"value1\": 45, \"value2\": 49 }')

    az dt twin relationship create -n $dtname --relationship 'has_twists' --twin-id $section --target $twist --relationship-id "$($section)_$($twist)"

    foreach ($number in $station_numbers) {

        $station = "station_$($number)"
        az dt twin relationship create -n $dtname --relationship 'has_stations' --twin-id $twist --target $station --relationship-id "$($twist)_$($station)"
    }
}

function New-VerticalVersineMeasure {
    param (
        $dtname,
        $section,
        $versine_number,
        $station_numbers
    )
    
    $versine = "v_versine_$($versine_number)"

    az dt twin create `
        -n $dtname `
        -m 'dtmi:railmonitoring:measurements:versine:vertical;3' `
        -t $versine `
        -p ('{ \"value1\": 12, \"value2\": 4 }')

    az dt twin relationship create -n $dtname --relationship 'has_vertical_versine' --twin-id $section --target $versine --relationship-id "$($section)_$($versine)"

    foreach ($number in $station_numbers) {

        $station = "station_$($number)"
        az dt twin relationship create -n $dtname --relationship 'has_stations' --twin-id $versine --target $station --relationship-id "$($versine)_$($station)"
    }
}

function New-HorizontalVersineMeasure {
    param (
        $dtname,
        $section,
        $versine_number,
        $station_numbers
    )
    
    $versine = "h_versine_$($versine_number)"

    az dt twin create `
        -n $dtname `
        -m 'dtmi:railmonitoring:measurements:versine:horizontal;2' `
        -t $versine `
        -p ('{ \"value1\": 3, \"value2\": 9 }')

    az dt twin relationship create -n $dtname --relationship 'has_horizontal_versine' --twin-id $section --target $versine --relationship-id "$($section)_$($versine)"

    foreach ($number in $station_numbers) {

        $station = "station_$($number)"
        az dt twin relationship create -n $dtname --relationship 'has_stations' --twin-id $versine --target $station --relationship-id "$($versine)_$($station)"
    }
}

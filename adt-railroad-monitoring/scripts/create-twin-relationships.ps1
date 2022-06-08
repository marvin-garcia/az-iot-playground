$dtname = 'railroadDT'

#region root
az dt twin create `
    -n $dtname `
    -m 'dtmi:railmonitoring:root;1' `
    -t 'root'

# tenant
az dt twin create `
    -n $dtname `
    -m 'dtmi:railmonitoring:tenant;1' `
    -t 'tenant_001' `
    -p '{\"TenantId\": \"000-000-001\",\"name\": \"Customer 001\" }'

az dt twin relationship create -n $dtname --relationship 'has_tenants' --twin-id 'root' --target 'tenant_001' --relationship-id 'root_tenant_001'
#endregion

#region tenant's project
az dt twin create `
    -n $dtname `
    -m 'dtmi:railmonitoring:project;1' `
    -t 'project_001' `
    -p '{\"id\": \"123-456-789\", \"name\": \"Project 001\", \"timeZoneOffset\": 2.0, \"timeZoneName\": \"South Africa Standard Time\", \"unitSettings\": \"0xA30000008b000001000\" }'

az dt twin relationship create -n $dtname --relationship 'has_projects' --twin-id 'tenant_001' --target 'project_001' --relationship-id 'tenant_001_project_001'
#endregion

#region project's section
az dt twin create `
    -n $dtname `
    -m 'dtmi:railmonitoring:section;1' `
    -t 'section_001' `
    -p '{ \"id\": \"123-456-789\", \"name\": \"Section 1\", \"cantBasis\": 4.5, \"firstStationDistance\": 231512, \"stationDistance\": 4.8 }'

az dt twin relationship create -n $dtname --relationship 'has_sections' --twin-id 'project_001' --target 'section_001' --relationship-id 'project_001_section_001'
#endregion

#region section's stations
New-TargetSensor -dtname $dtname -section 'section_001' -station_number 1 -sensor_numbers @(1, 2)
New-TargetSensor -dtname $dtname -section 'section_001' -station_number 2 -sensor_numbers @(3, 4)
New-TargetSensor -dtname $dtname -section 'section_001' -station_number 3 -sensor_numbers @(5, 6)
New-TiltSensor -dtname $dtname -section 'section_001' -station_number 4 -sensor_numbers @(7)
New-TargetSensor -dtname $dtname -section 'section_001' -station_number 5 -sensor_numbers @(8, 9)
#endregion

#region section's twist
New-TwistMeasure -dtname $dtname -section 'section_001' -twist_number 1 -station_numbers @(1, 2)
New-TwistMeasure -dtname $dtname -section 'section_001' -twist_number 2 -station_numbers @(2, 3)
New-TwistMeasure -dtname $dtname -section 'section_001' -twist_number 3 -station_numbers @(3, 4)
New-TwistMeasure -dtname $dtname -section 'section_001' -twist_number 4 -station_numbers @(4, 5)
#endregion

# vertical versines
New-VerticalVersineMeasure -dtname $dtname -section 'section_001' -versine_number 1 -station_numbers @(1, 2, 3)
New-VerticalVersineMeasure -dtname $dtname -section 'section_001' -versine_number 2 -station_numbers @(4, 5)

# horizontal versines
New-HorizontalVersineMeasure -dtname $dtname -section 'section_001' -versine_number 1 -station_numbers @(1, 2, 3)
New-HorizontalVersineMeasure -dtname $dtname -section 'section_001' -versine_number 2 -station_numbers @(4, 5)

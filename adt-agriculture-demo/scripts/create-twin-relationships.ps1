$dtname = '<adt-instance-name>'

az dt twin relationship create -n $dtname --relationship 'rel_has_tractors' --twin-id 'farm001' --target 'tractor001' --relationship-id 'farm001_tractor001'
az dt twin relationship create -n $dtname --relationship 'rel_has_tractors' --twin-id 'farm001' --target 'tractor002' --relationship-id 'farm001_tractor002'
az dt twin relationship create -n $dtname --relationship 'rel_has_tractors' --twin-id 'farm002' --target 'tractor003' --relationship-id 'farm002_tractor003'
az dt twin relationship create -n $dtname --relationship 'rel_has_tractors' --twin-id 'farm003' --target 'tractor004' --relationship-id 'farm003_tractor004'
az dt twin relationship create -n $dtname --relationship 'rel_has_tractors' --twin-id 'farm003' --target 'tractor005' --relationship-id 'farm003_tractor005'

az dt twin relationship create -n $dtname --relationship 'rel_has_farms' --twin-id 'customer001' --target 'farm001' --relationship-id 'customer001_farm001'
az dt twin relationship create -n $dtname --relationship 'rel_has_farms' --twin-id 'customer001' --target 'farm002' --relationship-id 'customer001_farm002'
az dt twin relationship create -n $dtname --relationship 'rel_has_farms' --twin-id 'customer002' --target 'farm003' --relationship-id 'customer002_farm003'
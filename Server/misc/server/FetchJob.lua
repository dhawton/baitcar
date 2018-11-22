ESX = nil

TriggerEvent('esx:getSharedObject', function(obj) ESX = obj end)
RegisterServerEvent('Baitcar.getJob')
AddEventHandler('Baitcar.getJob', function(source)
	local _source = source
	local xPlayer = ESX.GetPlayerFromId(_source)
	
	if xPlayer ~= nil and xPlayer.job ~= nil then
		TriggerClientEvent('Baitcar.hasJob', xPlayer.job.name)
	end
end)
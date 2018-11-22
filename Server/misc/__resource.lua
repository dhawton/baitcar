resource_manifest_version '44febabe-d386-4d18-afbe-5e627f4af937'

description 'Baitcar'

version '0.99.0'

server_scripts {
	'config.json',
	'server/Baitcar.Server.net.dll',
	'server/FetchJob.lua'
}

client_scripts {
	'config.json',
	'client/Baitcar.Client.net.dll'
}

dependencies {
    'es_extended',
    'esx_policeJob'
}
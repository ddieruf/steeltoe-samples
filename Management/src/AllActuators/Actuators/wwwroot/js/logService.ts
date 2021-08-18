﻿
class LogService
{
    public GetLogLevelsAndNamespaces() : Promise<IDynamicLogLevels> {
        return new Promise<IDynamicLogLevels>((resolve, reject) => {
            fetch("/actuator/loggers")
                .then((response: Response) => response.json())
                .then((data: IDynamicLogLevels) => {
                    resolve(data);
                })
                .catch(error => reject(error));
        });
    }

    public SetLogLevel(name: string, level: string) : Promise<ILogConfiguration>
    {
        return new Promise<ILogConfiguration>((resolve, reject) => {
            
            const requestBody: ILogRequest = { configuredLevel: level };

            fetch(`/actuator/loggers/${name}`, { method: 'POST', body: JSON.stringify(requestBody)})
                .then((response: Response) =>
                {
                    if(response.ok)
                    {
                        resolve({ effectiveLevel: level });
                    }
                    else {
                        reject({ reason: `unsuccessful request: ${response.status}`});
                    }
                })
                .catch(error => reject(error));
        });
    }
}

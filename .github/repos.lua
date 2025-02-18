local config = {
    labels_fetch = "repo-fetch,dependencies",
    labels_update = "dependencies"
}

local repos = {
    {
        name = "YarnProjects",
        url = "https://github.com/Peaceland-Game/Yarn-Spring-2025.git",
        dir = "Assets/Yarn/",
        def_branch = "main"
    }
}

-- Fully needed, so that the scripts can access the configurations.
return {
    repos = repos,
    config = config
}

#pragma once
class WarCardGame
{
public:
    enum GameMode
    {
        MODE_COMPUTER,
        MODE_SIMULATION,
    };

public:
    WarCardGame();
    ~WarCardGame();

    void Play(GameMode mode);
    
};


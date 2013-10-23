-- Dropping existing FOREIGN KEY constraints
-- NOTE: if the constraint does not exist, an ignorable error will be reported.
-- --------------------------------------------------

    ALTER TABLE [BookSet_CompSciBook] DROP CONSTRAINT [FK_CompSciStudentCompSciBook];
GO
    ALTER TABLE [StudentSet_CompSciStudent] DROP CONSTRAINT [FK_CompSciStudent_inherits_Student];
GO
    ALTER TABLE [BookSet_CompSciBook] DROP CONSTRAINT [FK_CompSciBook_inherits_Book];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- NOTE: if the table does not exist, an ignorable error will be reported.
-- --------------------------------------------------

    DROP TABLE [StudentSet];
GO
    DROP TABLE [BookSet];
GO
    DROP TABLE [StudentSet_CompSciStudent];
GO
    DROP TABLE [BookSet_CompSciBook];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'StudentSet'
CREATE TABLE [StudentSet] (
    [PersonId] int  NOT NULL,
    [Name] nvarchar(4000)  NOT NULL,
    [Address] nvarchar(4000)  NULL,
    [Phone] int  NULL
);
GO

-- Creating table 'BookSet'
CREATE TABLE [BookSet] (
    [ISBN] int  NOT NULL,
    [Title] nvarchar(4000)  NULL,
    [Author] nvarchar(4000)  NULL,
    [Description] nvarchar(4000)  NULL,
    [Pages] int  NULL,
    [Cost] decimal(29,29)  NULL
);
GO

-- Creating table 'StudentSet_CompSciStudent'
CREATE TABLE [StudentSet_CompSciStudent] (
    [HoursCoded] int  NULL,
    [NumProjects] int  NULL,
    [PersonId] int  NOT NULL,
    [Name] nvarchar(4000)  NOT NULL
);
GO

-- Creating table 'StudentSet_CompSciBook'
CREATE TABLE [StudentSet_CompSciBook] (
    [ACMAward] bit  NULL,
    [NumCodingExercises] int  NULL,
    [PersonId] int  NOT NULL,
    [Name] nvarchar(4000)  NOT NULL
);
GO

-- Creating table 'CompSciStudentBook'
CREATE TABLE [CompSciStudentBook] (
    [CompSciStudent_PersonId] int  NOT NULL,
    [CompSciStudent_Name] nvarchar(4000)  NOT NULL,
    [Books_ISBN] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [PersonId], [Name] in table 'StudentSet'
ALTER TABLE [StudentSet]
ADD CONSTRAINT [PK_StudentSet]
    PRIMARY KEY ([PersonId], [Name] );
GO

-- Creating primary key on [ISBN] in table 'BookSet'
ALTER TABLE [BookSet]
ADD CONSTRAINT [PK_BookSet]
    PRIMARY KEY ([ISBN] );
GO

-- Creating primary key on [PersonId], [Name] in table 'StudentSet_CompSciStudent'
ALTER TABLE [StudentSet_CompSciStudent]
ADD CONSTRAINT [PK_StudentSet_CompSciStudent]
    PRIMARY KEY ([PersonId], [Name] );
GO

-- Creating primary key on [PersonId], [Name] in table 'StudentSet_CompSciBook'
ALTER TABLE [StudentSet_CompSciBook]
ADD CONSTRAINT [PK_StudentSet_CompSciBook]
    PRIMARY KEY ([PersonId], [Name] );
GO

-- Creating primary key on [CompSciStudent_PersonId], [CompSciStudent_Name], [Books_ISBN] in table 'CompSciStudentBook'
ALTER TABLE [CompSciStudentBook]
ADD CONSTRAINT [PK_CompSciStudentBook]
    PRIMARY KEY ([CompSciStudent_PersonId], [CompSciStudent_Name], [Books_ISBN] );
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [CompSciStudent_PersonId], [CompSciStudent_Name] in table 'CompSciStudentBook'
ALTER TABLE [CompSciStudentBook]
ADD CONSTRAINT [FK_CompSciStudentBook_CompSciStudent]
    FOREIGN KEY ([CompSciStudent_PersonId], [CompSciStudent_Name])
    REFERENCES [StudentSet_CompSciStudent]
        ([PersonId], [Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Books_ISBN] in table 'CompSciStudentBook'
ALTER TABLE [CompSciStudentBook]
ADD CONSTRAINT [FK_CompSciStudentBook_Book]
    FOREIGN KEY ([Books_ISBN])
    REFERENCES [BookSet]
        ([ISBN])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CompSciStudentBook_Book'
CREATE INDEX [IX_FK_CompSciStudentBook_Book]
ON [CompSciStudentBook]
    ([Books_ISBN]);
GO

-- Creating foreign key on [PersonId], [Name] in table 'StudentSet_CompSciStudent'
ALTER TABLE [StudentSet_CompSciStudent]
ADD CONSTRAINT [FK_CompSciStudent_inherits_Student]
    FOREIGN KEY ([PersonId], [Name])
    REFERENCES [StudentSet]
        ([PersonId], [Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [PersonId], [Name] in table 'StudentSet_CompSciBook'
ALTER TABLE [StudentSet_CompSciBook]
ADD CONSTRAINT [FK_CompSciBook_inherits_CompSciStudent]
    FOREIGN KEY ([PersonId], [Name])
    REFERENCES [StudentSet_CompSciStudent]
        ([PersonId], [Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
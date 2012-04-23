class AddBuildingAbbreviationColumnAndBuildingType < ActiveRecord::Migration
  def self.up
    add_column :buildings, :type, :string
    add_column :buildings, :abbreviation, :string
  end

  def self.down
    remove_column :buildings, :type
    remove_column :buildings, :abbreviation
  end
end
